import { connect } from 'react-redux';
import { push } from 'connected-react-router';
import { createSelector } from 'reselect';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import SeriesSearchInput from './SeriesSearchInput';

function createCleanSeriesSelector() {
  return createSelector(
    createAllSeriesSelector(),
    createTagsSelector(),
    (allSeries, allTags) => {
      return allSeries.map((series) => {
        const {
          title,
          titleSlug,
          sortTitle,
          images,
          alternateTitles = [],
          tags = []
        } = series;

        return {
          title,
          titleSlug,
          sortTitle,
          images,
          alternateTitles,
          firstCharacter: title.charAt(0).toLowerCase(),
          tags: tags.map((id) => {
            return allTags.find((tag) => tag.id === id);
          })
        };
      });
    }
  );
}

function createMapStateToProps() {
  return createDeepEqualSelector(
    createCleanSeriesSelector(),
    (series) => {
      return {
        series
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onGoToSeries(titleSlug) {
      dispatch(push(`${window.Sonarr.urlBase}/series/${titleSlug}`));
    },

    onGoToAddNewSeries(query) {
      dispatch(push(`${window.Sonarr.urlBase}/add/new?term=${encodeURIComponent(query)}`));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(SeriesSearchInput);